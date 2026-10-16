// Stajyer detay sayfası: proje kartındaki "Geri Al" butonuyla açılan onay penceresi.
// intern-detail.js içindeki internProjects, internTasks ve loadDetails() burada kullanılır.

const revokeModalElement = document.getElementById("revokeProjectModal");
const revokeModal = new bootstrap.Modal(revokeModalElement);
const revokeName = document.getElementById("revokeProjectName");
const revokeWarning = document.getElementById("revokeProjectWarning");
const revokeError = document.getElementById("revokeProjectError");
const revokeConfirm = document.getElementById("revokeProjectConfirm");
const revokeCancel = document.getElementById("revokeProjectCancel");
const revokeNoticeBox = document.getElementById("noticeBox");

let projectToRevokeID = null;

function openRevokeModal(ongoingProjectID) {
    const project = internProjects.find(item => item.ongoingProjectID === ongoingProjectID);
    if (!project) return;

    projectToRevokeID = ongoingProjectID;
    revokeName.textContent = project.projectTitle;
    hideMessage(revokeError);
    revokeConfirm.disabled = false;
    revokeCancel.disabled = false;

    // API projeyle birlikte bağlı görevleri de siler. Sayı, sayfada zaten yüklü görevlerden hesaplanır.
    const taskCount = internTasks.filter(task => task.ongoingProjectID === ongoingProjectID).length;
    if (taskCount > 0) {
        showMessage(revokeWarning, "warning",
            `Bu proje için ${taskCount} görev atanmış. Projeyi geri alırsanız bu görevler de silinecek.`);
    } else {
        hideMessage(revokeWarning);
    }

    revokeModal.show();
}

async function confirmRevoke() {
    if (revokeConfirm.disabled || projectToRevokeID === null) return;   // çift tıklama koruması

    const title = internProjects.find(item => item.ongoingProjectID === projectToRevokeID)?.projectTitle ?? "";
    hideMessage(revokeError);
    revokeConfirm.disabled = true;
    revokeCancel.disabled = true;

    try {
        await revokeProject(projectToRevokeID);

        revokeModal.hide();
        showNotice(revokeNoticeBox, "success", `"${title}" projesi stajyerden geri alındı.`);
        await loadDetails();   // projeler ve (silinmiş olabilecek) görevler birlikte yenilenir
    } catch (error) {
        showError(revokeError, error);   // API'nin mesajı olduğu gibi gösterilir

        // Atama zaten yoksa ekrandaki liste eskimiş demektir; arkada tazelenir
        if (error instanceof ApiError && error.status === 404) {
            loadDetails();
        }
    } finally {
        revokeConfirm.disabled = false;
        revokeCancel.disabled = false;
    }
}

// ---------- Olay dinleyicileri ----------
document.getElementById("projectsList").addEventListener("click", (event) => {
    const button = event.target.closest("button[data-action=revoke]");
    if (!button || button.disabled) return;

    openRevokeModal(Number(button.dataset.ongoingProjectId));
});

revokeConfirm.addEventListener("click", confirmRevoke);