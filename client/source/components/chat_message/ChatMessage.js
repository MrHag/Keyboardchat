import React from 'react';

import './ChatMessage.scss';
const MessageBody = require('../../../../server/messageBody');

const ChatMessage = (props) => {
    const msg = MessageBody.fromJSON(props.msg);
    return (
        <div className="chat-msg">
            <div className="chat-msg__user">
                <img className="chat-msg__av" src={msg.avatar}></img>
                <span className="chat-msg__name">{msg.name}</span>
            </div>
            <div className="chat-msg__content">
                <span className="chat-msg__text">{msg.message}</span>
            </div>
        </div>
    )
}

export default ChatMessage;