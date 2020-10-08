import React from 'react';

import './ChatMessage.scss';

const ChatMessage = (props) => {
    const { name, avatar, message } = props.msg;
    return (
        <div className="chat-msg">
            <div className="chat-msg__user">
                <img className="chat-msg__av" src={avatar}></img>
                <span className="chat-msg__name">{name}</span>
            </div>
            <div className="chat-msg__content">
                <span className="chat-msg__text">{message}</span>
            </div>
        </div>
    )
}

export default ChatMessage;