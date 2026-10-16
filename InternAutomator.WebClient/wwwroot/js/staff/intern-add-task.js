// Stajyer detay sayfası: "Görevler > Ekle" ve görevdeki "Düzenle" ile açılan görev penceresi.
// Aynı form iki işlem için kullanılır. intern-detail.js içindeki internID, internProjects,
// internTasks ve loadDetails() burada kullanılır.

const taskAddButton = document.getElementById("tasksAdd");
const taskModalElement = document.getElementById("addTaskModal");
const taskModal = new bootstrap.Modal(taskModalElement);
const taskForm = document.getElementById("addTaskForm");
const taskError = document.getElementById("addTaskError");
const taskNoticeBox = document.getElementById("noticeBox");
const taskProjectSelect = document.getElementById("taskProjectSelect");
const taskTitleInput = document.getElementById("taskTitleInput");
const taskDescriptionInput = document.getElementById("taskDescriptionInput");
const taskDueDateInput = document.getElementById("taskDueDateInput");
const taskSubmit = document.getElementById("addTaskSubmit");
const taskCancel = document.getElementById("addTaskCancel");
const taskModalTitle = document.getElementById("addTaskTitle");

// null ise "yeni görev", dolu ise düzenlenen görevin numarasıdır
let editingTaskID = null;
let editingOriginal = null;   // düzenlenen görevin açılıştaki değerleri (değişiklik karşılaştırması için)

taskTitleInput.maxLength = TASK_LIMITS.title;
taskDescriptionInput.maxLength = TASK_LIMITS.description;

// Görevler gün bazlıdır; saat sorulmaz. Seçilen günün sonu (23:59:59) gönderilir, böylece
// API'nin "teslim zamanı şu andan sonra olmalı" kuralı bugün seçildiğinde de sağlanır.
// Saat bilgisi eklenmek istenirse sadece bu fonksiyon değişir.
function toTaskDueDate(dayText) {
    return `${dayText}T23:59:59`;
}

function setTaskBusy(isBusy) {
    // Düzenlemede görevin projesi değiştirilemez; liste her zaman pasif kalır
    taskProjectSelect.disabled = isBusy || editingTaskID !== null;
    taskTitleInput.disabled = isBusy;
    taskDescriptionInput.disabled = isBusy;
    taskDueDateInput.disabled = isBusy;
    taskCancel.disabled = isBusy;
    taskSubmit.disabled = isBusy;
}

// Proje listesi: stajyerin atanmış projeleri (görev ongoingProjectID ile bağlanır)
function fillTaskProjectSelect() {
    taskProjectSelect.replaceChildren();

    // Birden fazla proje varsa kullanıcı bilerek seçsin; tek proje varsa hazır gelir
    if (internProjects.length !== 1) {
        const placeholder = document.createElement("option");
        placeholder.value = "";
        placeholder.textContent = "Proje seçin";
        taskProjectSelect.appendChild(placeholder);
    }

    internProjects.forEach(project => {
        const option = document.createElement("option");
        option.value = project.ongoingProjectID;
        option.textContent = project.projectTitle;
        taskProjectSelect.appendChild(option);
    });
}

function openTaskModal() {
    editingTaskID = null;
    editingOriginal = null;
    taskModalTitle.textContent = "Görev Ekle";
    taskSubmit.textContent = "Görevi Ekle";

    taskForm.reset();
    hideMessage(taskError);
    setTaskBusy(false);

    fillTaskProjectSelect();
    taskDueDateInput.min = getTodayText();   // sayfa gece yarısını geçerse diye her açılışta yenilenir

    taskModal.show();
}

// Görev adı, açıklama ve teslim günü düzenlenebilir; proje değişmez
function openEditTaskModal(taskID) {
    const task = internTasks.find(item => item.taskID === taskID);
    if (!task) return;

    editingTaskID = taskID;
    editingOriginal = {
        title: (task.title ?? "").trim(),
        description: (task.description ?? "").trim(),
        dayText: (task.dueDate ?? "").slice(0, 10),   // "2026-12-01T23:59:59" -> "2026-12-01"
        dueDate: task.dueDate
    };
    taskModalTitle.textContent = "Görevi Düzenle";
    taskSubmit.textContent = "Kaydet";

    taskForm.reset();
    hideMessage(taskError);
    setTaskBusy(false);

    // Proje alanı sadece görevin projesini gösterir (pasif)
    taskProjectSelect.replaceChildren();
    const project = internProjects.find(item => item.ongoingProjectID === task.ongoingProjectID);
    const option = document.createElement("option");
    option.value = task.ongoingProjectID;
    option.textContent = project ? project.projectTitle : "-";
    taskProjectSelect.appendChild(option);

    taskTitleInput.value = editingOriginal.title;
    taskDescriptionInput.value = editingOriginal.description;
    taskDueDateInput.value = editingOriginal.dayText;
    taskDueDateInput.min = getTodayText();

    taskModal.show();
}

async function submitUpdate(title, dayText) {
    const description = taskDescriptionInput.value.trim();
    const isDateChanged = dayText !== editingOriginal.dayText;

    // Hiçbir şey değişmediyse sunucuya gitmeye gerek yok
    if (title === editingOriginal.title && description === editingOriginal.description && !isDateChanged) {
        taskModal.hide();
        return;
    }

    hideMessage(taskError);
    setTaskBusy(true);

    try {
        await updateTask(editingTaskID, {
            title,
            lastUpdatedByUserID: session.userID,
            description,
            // Tarih değişmediyse eski değer aynen gönderilir; saat bilgisi sessizce değişmesin
            dueDate: isDateChanged ? toTaskDueDate(dayText) : editingOriginal.dueDate
        });

        taskModal.hide();
        showNotice(taskNoticeBox, "success", `"${title}" görevi güncellendi.`);
        await loadDetails();
    } catch (error) {
        showError(taskError, error);   // API'nin mesajı olduğu gibi gösterilir

        // Görev artık yoksa ekrandaki liste eskimiş demektir; arkada tazelenir
        if (error instanceof ApiError && error.status === 404) {
            loadDetails();
        }
    } finally {
        setTaskBusy(false);
    }
}

async function submitTask(event) {
    event.preventDefault();
    if (taskSubmit.disabled) return;   // çift gönderim koruması

    const ongoingProjectID = Number(taskProjectSelect.value);
    const title = taskTitleInput.value.trim();
    const dayText = taskDueDateInput.value;

    // Asıl doğrulama API'dadır; buradakiler kullanıcıyı baştan uyarır.
    // min özelliği elle yazılan tarihi engellemez, bu yüzden tarih burada da kontrol edilir.
    let message = "";
    let invalidField = null;

    if (!ongoingProjectID) {
        message = "Lütfen bir proje seçin.";
        invalidField = taskProjectSelect;
    } else if (!title) {
        message = "Görev adı zorunludur.";
        invalidField = taskTitleInput;
    } else if (!dayText) {
        message = "Teslim tarihi zorunludur.";
        invalidField = taskDueDateInput;
    } else if (dayText < getTodayText() && (editingTaskID === null || dayText !== editingOriginal.dayText)) {
        // Düzenlemede tarih hiç değiştirilmediyse (gecikmiş görev) client engellemez; kararı API verir
        message = "Teslim tarihi bugünden önce olamaz.";
        invalidField = taskDueDateInput;
    } else if (!session.userID) {
        message = "Oturum bilgisi eksik. Lütfen çıkış yapıp tekrar giriş yapın.";
    }

    if (message) {
        showMessage(taskError, "danger", message);
        invalidField?.focus();
        return;
    }

    // Düzenleme modu: aynı doğrulamalardan sonra güncelleme isteği atılır
    if (editingTaskID !== null) {
        await submitUpdate(title, dayText);
        return;
    }

    hideMessage(taskError);
    setTaskBusy(true);

    try {
        await addTask({
            createdByUserID: session.userID,
            ongoingProjectID,
            title,
            description: taskDescriptionInput.value.trim(),
            dueDate: toTaskDueDate(dayText)
        });

        taskModal.hide();
        showNotice(taskNoticeBox, "success", `"${title}" görevi eklendi.`);
        await loadDetails();
    } catch (error) {
        showError(taskError, error);   // API'nin mesajı olduğu gibi gösterilir
    } finally {
        setTaskBusy(false);
    }
}

// ---------- Olay dinleyicileri ----------
taskAddButton.addEventListener("click", openTaskModal);
taskForm.addEventListener("submit", submitTask);

document.getElementById("tasksList").addEventListener("click", (event) => {
    const button = event.target.closest("button[data-action=edit]");
    if (!button || button.disabled) return;

    openEditTaskModal(Number(button.dataset.taskId));
});

taskModalElement.addEventListener("shown.bs.modal", () => {
    // Tek proje varsa doğrudan görev adına odaklan
    (taskProjectSelect.value ? taskTitleInput : taskProjectSelect).focus();
});