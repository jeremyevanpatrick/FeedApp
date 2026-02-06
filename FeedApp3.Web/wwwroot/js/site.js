$.validator.setDefaults({
    highlight: function (element) {
        $(element)
            .addClass('is-invalid')
            .removeClass('is-valid');
    },
    unhighlight: function (element) {
        $(element)
            .removeClass('is-invalid')
            .addClass('is-valid');
    }
});

function resetFormAjax(formId, errorMessageId) {
    const formElement = document.getElementById(formId);
    if (formElement) {
        formElement.reset();
        formElement.querySelectorAll('.is-valid, .is-error')?.forEach(el => {
            el.classList.remove('is-valid', 'is-error');
        });
        formElement.querySelectorAll('.field-validation-valid')?.forEach(span => {
            span.innerHTML = '';
        });
        const errorMessageElement = formElement.querySelector('#' + errorMessageId);
        if (errorMessageElement) {
            errorMessageElement.classList.add('d-none');
            errorMessageElement.innerHTML = '';
        }
    }
}

function resetFormSubmit(formId) {
    const formElement = document.getElementById(formId);
    if (formElement) {
        formElement.reset();
        formElement.querySelectorAll('.is-valid, .is-error')?.forEach(el => {
            el.classList.remove('is-valid', 'is-error');
        });
        formElement.querySelectorAll('.field-validation-valid')?.forEach(span => {
            span.innerHTML = '';
        });
        formElement.querySelector('.validationSummaryContainer')?.remove();
    }
}

async function addSubmitFormListener(formId, successCallback, failureCallback) {

    document.getElementById(formId).addEventListener('submit', async function (e) {
        e.preventDefault();

        if (!$("#"+formId).valid()) {
            return;
        }

        //show spinner
        const submitBtn = document.querySelector('#' + formId + ' button[type="submit"] span.spinner-border');
        submitBtn.classList.remove("d-none");

        const form = e.target;
        const data = new FormData(form);
        const token = document.querySelector('#' + formId + ' input[name="__RequestVerificationToken"]').value;

        const postUrl = document.getElementById(formId).getAttribute("action");

        const response = await fetch(postUrl, {
            method: 'POST',
            body: data,
            headers: {
                'X-Requested-With': 'XMLHttpRequest',
                'RequestVerificationToken': token
            }
        });
        
        if (response.ok) {
            successCallback();
        } else {
            var errorString = null;
            try {
                const responseJson = await response.json();
                errorString = responseJson.detail;
            } catch { }

            failureCallback(errorString ?? "An unexpected error occurred. Please try again later.");
        }

        //hide spinner
        submitBtn.classList.add("d-none");
    });

}

async function postJson(url, json, successCallback, failureCallback) {

    const token = document.querySelector('#app input[name="__RequestVerificationToken"]').value;

    const response = await fetch(url, {
        method: 'POST',
        body: JSON.stringify(json),
        headers: {
            'X-Requested-With': 'XMLHttpRequest',
            'Content-Type': 'application/json',
            'RequestVerificationToken': token
        }
    });

    if (response.ok) {
        successCallback();
    } else {
        var errorString = null;
        try {
            const responseJson = await response.json();
            errorString = responseJson.detail;
        } catch { }

        failureCallback(errorString ?? "An unexpected error occurred. Please try again later.");
    }

}