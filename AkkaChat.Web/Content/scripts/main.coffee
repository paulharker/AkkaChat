
highlightJs = ->
  hljs.tabReplace = '    ';
  hljs.initHighlightingOnLoad();


scrollToBottomOfChat = ->
    message_container = document.querySelector('.messages-container')
    message_container.scrollTop = message_container.scrollHeight

authed = false
window.toggleAuth = -> 
    authed = !authed

    if authed
        $(".messages-container").addClass("chatting")
        $(".chat-box").removeClass("hidden")
        $(".room-listing").removeClass("hidden")
        $(".signup-button").addClass("hidden")
        
    else
        $(".messages-container").removeClass("chatting")
        $(".chat-box").addClass("hidden")
        $(".room-listing").addClass("hidden")
        $(".signup-button").removeClass("hidden")
        
        
getCaret = (el) ->
  if el.selectionStart
    return el.selectionStart
  else if document.selection
    el.focus()
    r = document.selection.createRange()
    if r == null
      return 0
    re = el.createTextRange()
    rc = re.duplicate()
    re.moveToBookmark r.getBookmark()
    rc.setEndPoint 'EndToStart', re
    return rc.text.length
  0

    
window.listenForClickSubmit = (chatHub, btnSelector, msgSelector, room, user) ->
    $(btnSelector).click ->
        submitMessage(chatHub, msgSelector, room, user)
    
window.listenForEnterSubmit = (chatHub, selector, room, user) ->
    $(selector).keyup (event) ->
      if event.keyCode == 13 # enter key      
        content = @value
        caret = getCaret(this)
        
        if event.shiftKey
          @value = content.substring(0, caret - 1) + '\n' + content.substring(caret, content.length)
          event.stopPropagation()
        else
          submitMessage(chatHub, selector, room, user)
    
submitMessage = (chatHub, messageContainerSelector, room, user) ->
    msg = $(messageContainerSelector).val()
    $(messageContainerSelector).val('').focus()
    # call the StartCrawl method on the SignalR hub
    scrollToBottomOfChat()
    chatHub.server.send
      content: msg
      roomname: room
      userName: user
      
# DOC READY
$ ->
  if hljs? then highlightJs()
  scrollToBottomOfChat()
