const form = document.getElementById("applyForm");
const messageBox = document.getElementById("message");
const submitButton = form.querySelector("button[type=submit]");

function getValue(id) {
    return document.getElementById(id).value.trim();
}

// Opsiyonel alan boşsa boş metin yerine null gönder
function getValueOrNull(id) {
    const value = getValue(id);
    return value === "" ? null : value;
}

// Formdaki değerleri API'nın beklediği JSON şekline çevirir
function buildApplication() {
    return {
        personInfo: {
            firstName: getValue("firstName"),
            lastName: getValue("lastName"),
            email: getValue("email"),
            phone: getValue("phone"),
            address: getValueOrNull("address"),
            imagePath: ""
        },
        university: getValue("university"),
        department: getValue("department"),
        classYear: Number(getValue("classYear")),
        linkedinURL: getValueOrNull("linkedinUrl"),
        githubURL: getValueOrNull("githubUrl"),
        notes: ""
    };
}

form.addEventListener("submit", async (event) => {
    event.preventDefault();               // sayfa yenilenmesin
    hideMessage(messageBox);
    submitButton.disabled = true;         // çift tıklamayı engelle

    try {
        await submitApplication(buildApplication());
        showMessage(messageBox, "success", "Başvurunuz alındı. Teşekkür ederiz!");
        form.reset();
    } catch (error) {
        showError(messageBox, error);
    } finally {
        submitButton.disabled = false;
    }
});