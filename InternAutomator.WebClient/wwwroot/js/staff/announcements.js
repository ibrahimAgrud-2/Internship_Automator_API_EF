// Staff duyurular sayfası (Vue 3).
// Kural: ekrana yazılan her veri {{ }} ile yazılır; Vue bunu HTML olarak değil metin olarak basar.
// Katmanlar: HTML (Vue şablonu) -> bu dosya -> core/api.js. API adresini sadece api.js bilir.

// Menü ve üst bar eskisi gibi shell.js ile çizilir; giriş yoksa oturum sayfasına yönlendirir.
const session = renderStaffShell("announcements");

if (session) {
    const { createApp, ref, reactive, computed, onMounted } = Vue;

    // Renk sayısı announcements.css içindeki announcement-color-N sınıflarıyla aynı olmalı
    const ANNOUNCEMENT_COLOR_COUNT = 5;

    createApp({
        setup() {
            // ---------- Durum (state): ekran bunlara bakarak kendiliğinden çizilir ----------
            const announcements = ref([]);
            const isLoading = ref(true);
            const loadErrors = ref([]);
            const notice = ref(null);              // { type, text } ya da null

            // Yayınlama / düzenleme penceresi
            const form = reactive({ title: "", content: "" });
            const editingID = ref(null);           // null: yeni duyuru, dolu: düzenleme
            const isSaving = ref(false);
            const formErrors = ref([]);

            // Silme penceresi
            const announcementToDelete = ref(null);
            const isDeleting = ref(false);
            const deleteErrors = ref([]);

            // Şablondaki ref="..." alanları (DOM elemanlarına ulaşmak için)
            const formModalEl = ref(null);
            const deleteModalEl = ref(null);
            const titleInput = ref(null);
            const contentInput = ref(null);

            const limits = ANNOUNCEMENT_LIMITS;

            // Hesaplanan değerler: bağlı oldukları state değişince kendiliğinden yenilenir
            const hasAnnouncements = computed(() => announcements.value.length > 0);
            const isEditing = computed(() => editingID.value !== null);

            // Bootstrap pencereleri (Vue'nun izlemesine gerek yok, düz değişken)
            let formModal;
            let deleteModal;

            function colorClass(index) {
                return `announcement-color-${index % ANNOUNCEMENT_COLOR_COUNT}`;
            }

            // Birkaç saniye görünüp kaybolan bildirim
            let noticeTimer;
            function pushNotice(type, text) {
                notice.value = { type, text };
                clearTimeout(noticeTimer);
                noticeTimer = setTimeout(() => { notice.value = null; }, 6000);
            }

            // ---------- Liste ----------
            async function loadAnnouncements() {
                isLoading.value = true;
                loadErrors.value = [];
                announcements.value = [];

                try {
                    announcements.value = await getAllAnnouncements();
                } catch (error) {
                    loadErrors.value = getErrorMessages(error);
                } finally {
                    isLoading.value = false;
                }
            }

            // ---------- Yayınlama / düzenleme ----------
            function openCreateModal() {
                editingID.value = null;
                form.title = "";
                form.content = "";
                formErrors.value = [];
                isSaving.value = false;
                formModal.show();
            }

            function openEditModal(item) {
                editingID.value = item.announcementID;
                form.title = (item.title ?? "").trim();
                form.content = (item.content ?? "").trim();
                formErrors.value = [];
                isSaving.value = false;
                formModal.show();
            }

            async function submitUpdate(title, content) {
                const current = announcements.value.find(item => item.announcementID === editingID.value);

                // Hiçbir şey değişmediyse sunucuya gitmeye gerek yok
                if (current
                    && title === (current.title ?? "").trim()
                    && content === (current.content ?? "").trim()) {
                    formModal.hide();
                    return;
                }

                formErrors.value = [];
                isSaving.value = true;

                try {
                    await updateAnnouncement(editingID.value, { title, content });

                    formModal.hide();
                    pushNotice("success", `"${title}" duyurusu güncellendi.`);
                    await loadAnnouncements();
                } catch (error) {
                    formErrors.value = getErrorMessages(error);

                    // Duyuru artık yoksa ekrandaki liste eskimiş demektir; arkada tazelenir
                    if (error instanceof ApiError && error.status === 404) {
                        loadAnnouncements();
                    }
                } finally {
                    isSaving.value = false;
                }
            }

            async function submitForm() {
                // İstek sürerken ikinci gönderim (Enter'a art arda basma vb.) yapılmasın
                if (isSaving.value) return;

                const title = form.title.trim();
                const content = form.content.trim();

                if (!title) {
                    formErrors.value = ["Başlık zorunludur."];
                    titleInput.value.focus();
                    return;
                }
                if (!content) {
                    formErrors.value = ["Duyuru metni zorunludur."];
                    contentInput.value.focus();
                    return;
                }

                // Düzenleme modunda yayınlayan ve yayın tarihi değişmez; sadece başlık ve metin güncellenir
                if (isEditing.value) {
                    await submitUpdate(title, content);
                    return;
                }

                // Oturumda kullanıcı numarası yoksa istek hiç atılmaz
                if (!session.userID) {
                    formErrors.value = ["Oturum bilgisi eksik. Lütfen çıkış yapıp tekrar giriş yapın."];
                    return;
                }

                formErrors.value = [];
                isSaving.value = true;

                try {
                    // Yayın tarihini API belirler; istemci göndermez
                    await createAnnouncement({
                        createdByUserID: session.userID,
                        title,
                        content
                    });

                    formModal.hide();
                    pushNotice("success", `"${title}" duyurusu yayınlandı.`);
                    await loadAnnouncements();   // ekranda her zaman sunucudaki gerçek hal görünsün
                } catch (error) {
                    formErrors.value = getErrorMessages(error);
                } finally {
                    isSaving.value = false;
                }
            }

            // ---------- Silme ----------
            function openDeleteModal(item) {
                announcementToDelete.value = item;
                deleteErrors.value = [];
                isDeleting.value = false;
                deleteModal.show();
            }

            async function confirmDelete() {
                if (isDeleting.value || !announcementToDelete.value) return;   // çift tıklama koruması

                const { announcementID, title } = announcementToDelete.value;
                deleteErrors.value = [];
                isDeleting.value = true;

                try {
                    await deleteAnnouncement(announcementID);

                    deleteModal.hide();
                    pushNotice("success", `"${title}" duyurusu silindi.`);
                    await loadAnnouncements();
                } catch (error) {
                    deleteErrors.value = getErrorMessages(error);

                    // Duyuru zaten yoksa ekrandaki liste eskimiş demektir; arkada tazelenir
                    if (error instanceof ApiError && error.status === 404) {
                        loadAnnouncements();
                    }
                } finally {
                    isDeleting.value = false;
                }
            }

            // ---------- Sayfa açılışı ----------
            onMounted(() => {
                formModal = new bootstrap.Modal(formModalEl.value);
                deleteModal = new bootstrap.Modal(deleteModalEl.value);

                // Pencere açılınca imleç başlık kutusunda olsun
                formModalEl.value.addEventListener("shown.bs.modal", () => titleInput.value.focus());

                loadAnnouncements();
            });

            // Şablon yalnızca burada döndürülenleri görebilir
            return {
                announcements, isLoading, loadErrors, notice, hasAnnouncements,
                form, isEditing, isSaving, formErrors, limits,
                announcementToDelete, isDeleting, deleteErrors,
                formModalEl, deleteModalEl, titleInput, contentInput,
                formatDateTime, colorClass,
                openCreateModal, openEditModal, submitForm,
                openDeleteModal, confirmDelete
            };
        }
    }).mount("#app");
}