// Live Chat Widget
(function () {
    var chatOpen = false;
    var chatWidget = null;

    function createWidget() {
        // Floating button
        var btn = document.createElement('div');
        btn.id = 'liveChatBtn';
        btn.innerHTML = '<i class="bi bi-chat-dots-fill"></i>';
        btn.style.cssText = 'position:fixed;bottom:20px;right:20px;width:56px;height:56px;background:#0d6efd;color:#fff;border-radius:50%;display:flex;align-items:center;justify-content:center;font-size:24px;cursor:pointer;z-index:1050;box-shadow:0 4px 12px rgba(0,0,0,0.3);transition:transform 0.2s';
        btn.onmouseenter = function () { btn.style.transform = 'scale(1.1)'; };
        btn.onmouseleave = function () { btn.style.transform = 'scale(1)'; };
        btn.onclick = toggleChat;
        document.body.appendChild(btn);

        // Chat panel
        var panel = document.createElement('div');
        panel.id = 'liveChatPanel';
        panel.style.cssText = 'position:fixed;bottom:90px;right:20px;width:340px;max-height:440px;background:#fff;border-radius:12px;box-shadow:0 8px 30px rgba(0,0,0,0.2);z-index:1050;display:none;flex-direction:column;overflow:hidden';
        panel.innerHTML =
            '<div style="background:#0d6efd;color:#fff;padding:14px 16px;display:flex;justify-content:space-between;align-items:center">' +
            '<div><i class="bi bi-headset me-2"></i><strong>' + T('LiveChat.Title') + '</strong></div>' +
            '<button onclick="toggleChat()" style="background:none;border:none;color:#fff;font-size:18px;cursor:pointer"><i class="bi bi-x-lg"></i></button>' +
            '</div>' +
            '<div style="padding:16px;flex:1;overflow-y:auto" id="chatMessages">' +
            '<div class="text-center text-muted small py-3"><i class="bi bi-shield-check me-1"></i>' + T('LiveChat.Welcome') + '</div>' +
            '</div>' +
            '<div style="padding:12px;border-top:1px solid #eee">' +
            '<div class="mb-2" id="chatNameRow"><input type="text" class="form-control form-control-sm" id="chatName" placeholder="' + T('LiveChat.Name') + '"></div>' +
            '<div class="mb-2" id="chatEmailRow"><input type="email" class="form-control form-control-sm" id="chatEmail" placeholder="' + T('LiveChat.Email') + '"></div>' +
            '<div class="input-group">' +
            '<input type="text" class="form-control form-control-sm" id="chatInput" placeholder="' + T('LiveChat.Placeholder') + '">' +
            '<button class="btn btn-primary btn-sm" onclick="sendChatMessage()"><i class="bi bi-send"></i></button>' +
            '</div></div>';
        document.body.appendChild(panel);
        chatWidget = panel;

        // If logged in, hide name/email fields
        if (isLoggedIn()) {
            var nameRow = document.getElementById('chatNameRow');
            var emailRow = document.getElementById('chatEmailRow');
            if (nameRow) nameRow.style.display = 'none';
            if (emailRow) emailRow.style.display = 'none';
        }

        // Enter key to send
        var input = document.getElementById('chatInput');
        if (input) {
            input.addEventListener('keypress', function (e) {
                if (e.key === 'Enter') sendChatMessage();
            });
        }
    }

    window.toggleChat = function () {
        chatOpen = !chatOpen;
        if (chatWidget) {
            chatWidget.style.display = chatOpen ? 'flex' : 'none';
        }
        var btn = document.getElementById('liveChatBtn');
        if (btn) {
            btn.innerHTML = chatOpen ? '<i class="bi bi-x-lg"></i>' : '<i class="bi bi-chat-dots-fill"></i>';
        }
    };

    window.sendChatMessage = function () {
        var input = document.getElementById('chatInput');
        var msg = input ? input.value.trim() : '';
        if (!msg) return;

        var name = '';
        var email = '';
        var user = getUser();

        if (user) {
            name = user.firstName + ' ' + (user.lastName || '');
            email = user.email;
        } else {
            var nameEl = document.getElementById('chatName');
            var emailEl = document.getElementById('chatEmail');
            name = nameEl ? nameEl.value.trim() : '';
            email = emailEl ? emailEl.value.trim() : '';
            if (!name || !email) {
                toastr.warning(T('LiveChat.FillFields'));
                return;
            }
        }

        // Show message in chat
        var messages = document.getElementById('chatMessages');
        messages.innerHTML += '<div class="text-end mb-2"><span class="badge bg-primary text-wrap text-start" style="max-width:80%;display:inline-block">' + escapeHtml(msg) + '</span></div>';
        messages.scrollTop = messages.scrollHeight;
        input.value = '';

        // Send to API
        $.ajax({
            url: apiBaseUrl + '/api/support',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                name: name,
                email: email,
                message: msg,
                subject: 'LiveChat'
            })
        }).done(function () {
            messages.innerHTML += '<div class="text-start mb-2"><span class="badge bg-light text-dark text-wrap text-start" style="max-width:80%;display:inline-block"><i class="bi bi-check-circle text-success me-1"></i>' + T('LiveChat.Received') + '</span></div>';
            messages.scrollTop = messages.scrollHeight;
        }).fail(function () {
            messages.innerHTML += '<div class="text-start mb-2"><span class="badge bg-danger text-wrap" style="max-width:80%;display:inline-block">' + T('LiveChat.Error') + '</span></div>';
            messages.scrollTop = messages.scrollHeight;
        });
    };

    function escapeHtml(text) {
        var div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    // Init when DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function () { setTimeout(createWidget, 500); });
    } else {
        setTimeout(createWidget, 500);
    }
})();
