document.getElementById('passwordless-login').style.display = 'none';

// Add a secret key combo to unhide the passwordless login button
document.addEventListener('keydown', function (e) {
    // keyCode 80 represents 'P', ctrlKey checks if 'ctrl' is pressed and altKey checks for 'alt'
    if (e.keyCode == 80 && e.ctrlKey && e.altKey) {
        // prevent default browser action
        e.preventDefault();
        // your code here to unhide HTML element

        console.log('You entered the secret code! Showing the passwordless login button.');

        let element = document.getElementById('passwordless-login');
        if (element) {
            element.style.display = 'inline-block';
        }
    }
});

document.getElementById('passwordless-login').addEventListener('click', function (event) {
    event.preventDefault();
    handleSignInSubmit();
});


async function handleSignInSubmit(event) {
    //event.preventDefault();

    //let username = this.username.value;

    // prepare form post data
    var formData = new FormData();
    //formData.append('username', username);

    // send to server for registering
    let makeAssertionOptions;
    try {
        var res = await fetch('/assertionOptions', {
            method: 'POST', // or 'PUT'
            body: formData, // data can be `string` or {object}!
            headers: {
                'Accept': 'application/json'
            }
        });

        makeAssertionOptions = await res.json();
    } catch (e) {
        showError("Request to server failed", e);
    }

    console.log("Assertion Options Object", makeAssertionOptions);

    // show options error to user
    if (makeAssertionOptions.status !== "ok") {
        console.log("Error creating assertion options");
        console.log(makeAssertionOptions.errorMessage);
        showError('Error', makeAssertionOptions.errorMessage);
        return;
    }

    // todo: switch this to coercebase64
    const challenge = makeAssertionOptions.challenge.replace(/-/g, "+").replace(/_/g, "/");
    makeAssertionOptions.challenge = Uint8Array.from(atob(challenge), c => c.charCodeAt(0));

    // fix escaping. Change this to coerce
    makeAssertionOptions.allowCredentials.forEach(function (listItem) {
        var fixedId = listItem.id.replace(/\_/g, "/").replace(/\-/g, "+");
        listItem.id = Uint8Array.from(atob(fixedId), c => c.charCodeAt(0));
    });

    console.log("Assertion options", makeAssertionOptions);
    

    showSuccess('Logging In...', 'Tap your security key to login.', '/images/securitykey.min.svg');

    // ask browser for credentials (browser will ask connected authenticators)
    let credential;
    try {
        credential = await navigator.credentials.get({ publicKey: makeAssertionOptions })
    } catch (err) {
        showError('Error', err.message ? err.message : err);
    }

    try {
        await verifyAssertionWithServer(credential);
    } catch (e) {
        showError("Could not verify assertion", e);
    }
}

/**
 * Sends the credential to the the FIDO2 server for assertion
 * @param {any} assertedCredential
 */
async function verifyAssertionWithServer(assertedCredential) {

    // Move data into Arrays incase it is super long
    let authData = new Uint8Array(assertedCredential.response.authenticatorData);
    let clientDataJSON = new Uint8Array(assertedCredential.response.clientDataJSON);
    let rawId = new Uint8Array(assertedCredential.rawId);
    let sig = new Uint8Array(assertedCredential.response.signature);
    let userHandle = new Uint8Array(assertedCredential.response.userHandle)
    const data = {
        id: assertedCredential.id,
        rawId: coerceToBase64Url(rawId),
        type: assertedCredential.type,
        extensions: assertedCredential.getClientExtensionResults(),
        response: {
            authenticatorData: coerceToBase64Url(authData),
            clientDataJSON: coerceToBase64Url(clientDataJSON),
            userHandle: userHandle !== null ? coerceToBase64Url(userHandle) : null,
            signature: coerceToBase64Url(sig)
        }
    };

    let response;
    try {
        let res = await fetch("/makeAssertion", {
            method: 'POST', // or 'PUT'
            body: JSON.stringify(data), // data can be `string` or {object}!
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            }
        });

        response = await res.json();
    } catch (e) {
        showError("Request to server failed", e);
        throw e;
    }

    console.log("Assertion Object", response);

    // show error
    if (response.status !== "ok") {
        console.log("Error doing assertion");
        console.log(response.errorMessage);
        showError(response.errorMessage);
        return;
    }

    showSuccess('Logged In!', 'You\'re logged in successfully.', null, 2000);

    // Now that the user is logged in, refresh the page
    // When you hit IdentityServer's login page and you are logged in, you will just be redirected back to the RP with an authorization code
    // after successful response from makeAssertion endpoint
    setTimeout(function () {
        // Get ReturnUrl from URL parameters
        const urlParams = new URLSearchParams(window.location.search);
        const returnUrl = urlParams.get('ReturnUrl');

        if (returnUrl) {
            // Decode the ReturnUrl to get the actual URL
            const decodedUrl = decodeURIComponent(returnUrl);

            // Redirect to the decoded URL
            window.location.href = decodedUrl;
        } else {
            // Fallback - refresh page if no ReturnUrl found
            location.reload(true);
        }
    }, 2000);
    
    // redirect?
    //window.location.href = "/dashboard/" + state.user.displayName;
}
