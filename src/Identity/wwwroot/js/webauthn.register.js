document.getElementById('register').addEventListener('submit', handleRegisterSubmit);

async function handleRegisterSubmit(event) {
    event.preventDefault();


    // possible values: none, direct, indirect
    let attestation_type = "none";
    // possible values: <empty>, platform, cross-platform
    let authenticator_attachment = "";

    // possible values: preferred, required, discouraged
    let user_verification = "preferred";

    // possible values: discouraged, preferred, required
    // NOTE: For usernameless scenarios, resident key must be set to true.
    let residentKey = "required";



    // prepare form post data
    var data = new FormData();
    //data.append('username', username);
    data.append('attType', attestation_type);
    data.append('authType', authenticator_attachment);
    data.append('userVerification', user_verification);
    data.append('residentKey', residentKey);

    // send to server for registering
    let makeCredentialOptions;
    try {
        makeCredentialOptions = await fetchMakeCredentialOptions(data);

    } catch (e) {
        console.error(e);
        let msg = "Something went really wrong";
        showError(msg);
    }


    console.log("Credential Options Object", makeCredentialOptions);

    if (makeCredentialOptions.status !== "ok") {
        console.log("Error creating credential options");
        console.log(makeCredentialOptions.errorMessage);
        showError(makeCredentialOptions.errorMessage);
        return;
    }

    // Turn the challenge back into the accepted format of padded base64
    makeCredentialOptions.challenge = coerceToArrayBuffer(makeCredentialOptions.challenge);
    // Turn ID into a UInt8Array Buffer for some reason
    makeCredentialOptions.user.id = coerceToArrayBuffer(makeCredentialOptions.user.id);

    makeCredentialOptions.excludeCredentials = makeCredentialOptions.excludeCredentials.map((c) => {
        c.id = coerceToArrayBuffer(c.id);
        return c;
    });

    if (makeCredentialOptions.authenticatorSelection.authenticatorAttachment === null) makeCredentialOptions.authenticatorSelection.authenticatorAttachment = undefined;

    console.log("Credential Options Formatted", makeCredentialOptions);

    //Swal.fire({
    //    title: 'Registering...',
    //    text: 'Tap your security key to finish registration.',
    //    imageUrl: "/images/securitykey.min.svg",
    //    showCancelButton: true,
    //    showConfirmButton: false,
    //    focusConfirm: false,
    //    focusCancel: false
    //});

    showSuccess('Registering...', 'Tap your security key to finish registration.', '/images/securitykey.min.svg');


    console.log("Creating PublicKeyCredential...");

    let newCredential;
    try {
        newCredential = await navigator.credentials.create({
            publicKey: makeCredentialOptions
        });
    } catch (e) {
        var msg = "Could not create credentials in browser. Probably because the username is already registered with your authenticator. Please change username or authenticator."
        console.error(msg, e);
        //showErrorAlert(msg, e);
        showError(msg, e);
    }


    console.log("PublicKeyCredential Created", newCredential);

    try {
        registerNewCredential(newCredential);

    } catch (e) {
        //showErrorAlert(err.message ? err.message : err);
        showError(err.message ? err.message : err);
    }
}

async function fetchMakeCredentialOptions(formData) {
    let response = await fetch('/makeCredentialOptions', {
        method: 'POST', // or 'PUT'
        body: formData, // data can be `string` or {object}!
        headers: {
            'Accept': 'application/json'
        }
    });

    let data = await response.json();

    return data;
}


// This should be used to verify the auth data with the server
async function registerNewCredential(newCredential) {
    // Move data into Arrays incase it is super long
    let attestationObject = new Uint8Array(newCredential.response.attestationObject);
    let clientDataJSON = new Uint8Array(newCredential.response.clientDataJSON);
    let rawId = new Uint8Array(newCredential.rawId);

    const data = {
        id: newCredential.id,
        rawId: coerceToBase64Url(rawId),
        type: newCredential.type,
        extensions: newCredential.getClientExtensionResults(),
        response: {
            attestationObject: coerceToBase64Url(attestationObject),
            clientDataJSON: coerceToBase64Url(clientDataJSON)
        }
    };

    let response;
    try {
        response = await registerCredentialWithServer(data);
    } catch (e) {
        showError('Error', e);
    }

    console.log("Credential Object", response);

    // show error
    if (response.status !== "ok") {
        console.log("Error creating credential");
        console.log(response.errorMessage);
        showError('Error', response.errorMessage);
        return;
    }

    // show success 
    //Swal.fire({
    //    title: 'Registration Successful!',
    //    text: 'You\'ve registered successfully.',
    //    type: 'success',
    //    timer: 2000
    //});

    showSuccess('Registration Successful!', 'You\'ve registered successfully.', null, 2000);

    // redirect to dashboard?
    //window.location.href = "/dashboard/" + state.user.displayName;
}

async function registerCredentialWithServer(formData) {
    let stringBody = JSON.stringify(formData);

    console.log("Registering Credential With Server...");
    console.log(stringBody);
    
    let response = await fetch('/makeCredential', {
        method: 'POST', // or 'PUT'
        body: stringBody, // data can be `string` or {object}!
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        }
    });

    let data = await response.json();

    return data;
}
