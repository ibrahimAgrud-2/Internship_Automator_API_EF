const session = renderStaffShell("messages");

// Şimdilik tek sohbet var: Stajyerler Ortak Grup. Sohbet listesi endpoint'i gelince bu numara oradan okunacak.
const GROUP_CONVERSATION_ID = 1;
const MESSAGE_COUNT = 50;

const chatStatus = document.getElementById("chatStatus");
const messageList = document.getElementById("messageList");
const groupPreview = document.getElementById("groupPreview");
const groupTime = document.getElementById("groupTime");

// Mesaj yazma satırı
const composerForm = document.getElementById("composerForm");
const messageInput = document.getElementById("messageInput");
const sendButton = document.getElementById("sendButton");
const sendError = document.getElementById("sendError");

messageInput.maxLength = MESSAGE_LIMITS.content;
// Stajyer adları kişi numarasına göre sabit bir renkte görünür
const SENDER_COLORS = ["text-success", "text-primary", "text-danger", "text-warning-emphasis", "text-info-emphasis", "text-secondary-emphasis"];

// ---------- Yardımcılar ----------
function formatTime(isoDate) {
    return new Date(isoDate).toLocaleTimeString("tr-TR", { hour: "2-digit", minute: "2-digit" });
}

// "Bugün", "Dün" ya da "19 Ekim 2026"
function formatDayLabel(isoDate) {
    const day = new Date(isoDate);
    const today = new Date();
    const yesterday = new Date();
    yesterday.setDate(today.getDate() - 1);

    if (day.toDateString() === today.toDateString()) return "Bugün";
    if (day.toDateString() === yesterday.toDateString()) return "Dün";
    return day.toLocaleDateString("tr-TR", { dateStyle: "long" });
}

// API'da isUser true ise mesajı personel yazmıştır, false ise stajyer
function isFromStaff(message) {
    return message.isStaff === true;
}

// Personelin adı gösterilmez (şimdilik stajyer - sistem yazışması); stajyerin adı gösterilir
function getSenderLabel(message) {
    return isFromStaff(message) ? "Personel" : (message.personFullName || "Stajyer");
}

// ---------- Mesaj balonu (veri her zaman textContent ile yazılır) ----------
function createMessageBubble(message) {
    const fromStaff = isFromStaff(message);

    const bubble = document.createElement("div");
    bubble.className = `chat-bubble ${fromStaff ? "from-staff" : "from-intern"}`;

    const sender = document.createElement("div");
    sender.className = "chat-sender " + (fromStaff
        ? "text-secondary"
        : SENDER_COLORS[Math.abs(Number(message.personID) || 0) % SENDER_COLORS.length]);
    sender.textContent = getSenderLabel(message);
    const text = document.createElement("div");
    text.className = "chat-text text-break";
    text.textContent = message.content;

    const time = document.createElement("div");
    time.className = "chat-time";
    time.textContent = formatTime(message.createdDate);

    bubble.append(sender, text, time);
    return bubble;
}

function createDaySeparator(isoDate) {
    const separator = document.createElement("div");
    separator.className = "chat-day";
    separator.textContent = formatDayLabel(isoDate);
    return separator;
}

// Mesajlar eskiden yeniye gelir; gün değişince ayraç eklenir
function renderMessages(messages) {
    const items = [];
    let lastDay = null;

    messages.forEach(message => {
        const day = new Date(message.createdDate).toDateString();
        if (day !== lastDay) {
            items.push(createDaySeparator(message.createdDate));
            lastDay = day;
        }
        items.push(createMessageBubble(message));
    });

    messageList.replaceChildren(...items);
    messageList.scrollTop = messageList.scrollHeight;   // en yeni mesaj görünsün
}

// Soldaki sohbet satırında son mesajın önizlemesi
function renderPreview(messages) {
    const last = messages[messages.length - 1];

    if (!last) {
        groupPreview.textContent = "Henüz mesaj yok";
        groupTime.textContent = "";
        return;
    }

    groupPreview.textContent = `${getSenderLabel(last)}: ${last.content}`;
    groupTime.textContent = formatTime(last.createdDate);
}

// showLoading false ise (mesaj gönderdikten sonra yenileme) liste silinip yükleniyor yazısı çıkmaz
async function loadMessages(showLoading = true) {
    if (showLoading) {
        messageList.replaceChildren();
        showMessage(chatStatus, "info", "Mesajlar yükleniyor...");
    }

    try {
// ... geri kalan aynı
        const messages = await getConversationMessages(GROUP_CONVERSATION_ID, MESSAGE_COUNT);

        renderPreview(messages);

        if (messages.length === 0) {
            showMessage(chatStatus, "secondary", "Henüz mesaj yok.");
            return;
        }

        hideMessage(chatStatus);
        renderMessages(messages);
    } catch (error) {
        groupPreview.textContent = "";
        showError(chatStatus, error);
    }
}
// ---------- Mesaj gönderme ----------
// Metin boşken ya da istek sürerken Gönder pasif olur
let isSending = false;

function updateSendButton() {
    sendButton.disabled = isSending || messageInput.value.trim() === "";
}

async function sendMessage(event) {
    event.preventDefault();

    // İstek sürerken ikinci gönderim (Enter'a art arda basma vb.) yapılmasın
    if (isSending) return;

    const content = messageInput.value.trim();
    if (!content) return;

    // Oturumda kullanıcı numarası yoksa istek hiç atılmaz
    if (!session.userID) {
        showMessage(sendError, "danger", "Oturum bilgisi eksik. Lütfen çıkış yapıp tekrar giriş yapın.");
        return;
    }

    hideMessage(sendError);
    isSending = true;
    messageInput.disabled = true;
    updateSendButton();

    try {
        await staffAddMessage({
            userID: session.userID,
            conversationID: GROUP_CONVERSATION_ID,
            content
        });

        messageInput.value = "";
        await loadMessages(false);   // ekranda her zaman sunucudaki gerçek hal görünsün
    } catch (error) {
        showError(sendError, error);   // yazılan metin silinmez, tekrar denenebilir
    } finally {
        isSending = false;
        messageInput.disabled = false;
        updateSendButton();
        messageInput.focus();
    }
}

composerForm.addEventListener("submit", sendMessage);
messageInput.addEventListener("input", updateSendButton);

// Enter gönderir, Shift+Enter yeni satır ekler
messageInput.addEventListener("keydown", (event) => {
    if (event.key === "Enter" && !event.shiftKey && !event.isComposing) {
        event.preventDefault();
        composerForm.requestSubmit();
    }
});
if (session) {
    loadMessages();
}