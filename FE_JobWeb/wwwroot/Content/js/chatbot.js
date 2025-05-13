
/*---------------------------------*/
const chatMessages = document.getElementById('chat-messages');
const userInput = document.getElementById('user-input');
const sendButton = document.getElementById('send-button');

function cleanMarkdown(text) {
    return text
        .replace(/#{1,6}\s?/g, '')
        .replace(/\*\*/g, '')
        .replace(/\n{3,}/g, '\n\n')
        .trim();
}

function addMessage(message, isUser) {
    const messageElement = document.createElement('div');
    messageElement.classList.add('message');
    messageElement.classList.add(isUser ? 'user-message' : 'bot-message');

    const profileImage = document.createElement('img');
    profileImage.classList.add('profile-image');
    profileImage.src = isUser ? '/Content/images/user.jpg' : '/Content/images/bot.jpg';
    profileImage.alt = isUser ? 'User' : 'Bot';

    const messageContent = document.createElement('div');
    messageContent.classList.add('message-content');
    messageContent.textContent = message;

    messageElement.appendChild(profileImage);
    messageElement.appendChild(messageContent);
    chatMessages.appendChild(messageElement);
    chatMessages.scrollTop = chatMessages.scrollHeight;

    return messageContent;
}

async function handleUserInput() {
    const userMessage = userInput.value.trim();
    if (userMessage) {
        addMessage(userMessage, true);
        userInput.value = '';
        userInput.style.height = 'auto';
        sendButton.disabled = true;
        userInput.disabled = true;

        // Thêm dòng "..." có hiệu ứng động
        const loadingMessageContent = addMessage('.', false);

        // Bắt đầu hiệu ứng typing "..." động
        let dotCount = 1;
        let loading = true;

        const intervalId = setInterval(() => {
            loadingMessageContent.textContent = '.'.repeat(dotCount);
            dotCount = dotCount === 3 ? 1 : dotCount + 1;
        }, 500);

        try {
            // Giả lập phản hồi bot (delay 2.5s)
            await new Promise(resolve => setTimeout(resolve, 2500));
            const botMessage = `Bạn vừa nói: "${userMessage}". Đây là phản hồi mẫu từ chatbot.`;

            // Dừng hiệu ứng và thay thế bằng nội dung thật
            clearInterval(intervalId);
            loading = false;
            loadingMessageContent.textContent = cleanMarkdown(botMessage);
        } catch (error) {
            clearInterval(intervalId);
            loading = false;
            loadingMessageContent.textContent = 'Đã xảy ra lỗi. Vui lòng thử lại sau.';
        } finally {
            sendButton.disabled = false;
            userInput.disabled = false;
            userInput.focus();
        }
    }
}

sendButton.addEventListener('click', handleUserInput);
userInput.addEventListener('keypress', (e) => {
    if (e.key === 'Enter' && !e.shiftKey) {
        e.preventDefault();
        handleUserInput();
    }
});

const maxLines = 3;
const lineHeight = 24; // điều chỉnh theo line-height của bạn

userInput.addEventListener('input', () => {
    userInput.style.height = 'auto'; // reset lại
    const lines = userInput.value.split('\n').length;
    const newHeight = Math.min(userInput.scrollHeight, maxLines * lineHeight);
    userInput.style.height = `${newHeight}px`;
});

const chatContainer = document.getElementById('chat-container');
const chatIcon = document.getElementById('chat-icon');

// Hàm để mở/đóng chatbot
chatIcon.addEventListener('click', () => {
    if (chatContainer.style.display === 'none' || !chatContainer.style.display) {
        chatContainer.style.display = 'flex'; // Hiện chatbot
    } else {
        chatContainer.style.display = 'none'; // Ẩn chatbot
    }
});

function toggleChat() {
    const popup = document.getElementById('chat-container');
    popup.style.display = popup.style.display === 'flex' ? 'none' : 'flex';

    // Nếu hiện hộp thoại, cập nhật thời gian tin nhắn chào
    if (popup.style.display === 'flex') {
        document.getElementById('welcomeTime').innerText = getCurrentTime();
    }
}
