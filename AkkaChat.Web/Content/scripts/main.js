(function() {
  var authed, getCaret, highlightJs, scrollToBottomOfChat, submitMessage;

  highlightJs = function() {
    hljs.tabReplace = '    ';
    return hljs.initHighlightingOnLoad();
  };

  scrollToBottomOfChat = function() {
    var message_container;
    message_container = document.querySelector('.messages-container');
    return message_container.scrollTop = message_container.scrollHeight;
  };

  authed = false;

  window.toggleAuth = function() {
    authed = !authed;
    if (authed) {
      $(".messages-container").addClass("chatting");
      $(".chat-box").removeClass("hidden");
      $(".room-listing").removeClass("hidden");
      return $(".signup-button").addClass("hidden");
    } else {
      $(".messages-container").removeClass("chatting");
      $(".chat-box").addClass("hidden");
      $(".room-listing").addClass("hidden");
      return $(".signup-button").removeClass("hidden");
    }
  };

  getCaret = function(el) {
    var r, rc, re;
    if (el.selectionStart) {
      return el.selectionStart;
    } else if (document.selection) {
      el.focus();
      r = document.selection.createRange();
      if (r === null) {
        return 0;
      }
      re = el.createTextRange();
      rc = re.duplicate();
      re.moveToBookmark(r.getBookmark());
      rc.setEndPoint('EndToStart', re);
      return rc.text.length;
    }
    return 0;
  };

  window.listenForClickSubmit = function(chatHub, btnSelector, msgSelector, room, user) {
    return $(btnSelector).click(function() {
      return submitMessage(chatHub, msgSelector, room, user);
    });
  };

  window.listenForEnterSubmit = function(chatHub, selector, room, user) {
    return $(selector).keyup(function(event) {
      var caret, content;
      if (event.keyCode === 13) {
        content = this.value;
        caret = getCaret(this);
        if (event.shiftKey) {
          this.value = content.substring(0, caret - 1) + '\n' + content.substring(caret, content.length);
          return event.stopPropagation();
        } else {
          return submitMessage(chatHub, selector, room, user);
        }
      }
    });
  };

  submitMessage = function(chatHub, messageContainerSelector, room, user) {
    var msg;
    msg = $(messageContainerSelector).val();
    $(messageContainerSelector).val('').focus();
      scrollToBottomOfChat();
    return chatHub.server.send({
      content: msg,
      roomname: room,
      userName: user
    });
  };

  $(function() {
    if (typeof hljs !== "undefined" && hljs !== null) {
      highlightJs();
    }
    return scrollToBottomOfChat();
  });

}).call(this);

//# sourceMappingURL=main.js.map
