import React from 'react';
import dateFnsFormat from  'date-fns/format'
import './ChatMessage.scss';

const ChatMessage = (props) => {
    const { date, name, avatar, message, userid } = props.msg;
    return (
        <div className="chat-msg">
            <div className="chat-msg__info">
                <img className="chat-msg__av" src={avatar}></img>
                <div className="chat-msg__name-date">
                    <span className="chat-msg__name">{`$${userid}`}</span>
                    <span className="chat-msg__date" title={date}>{dateFnsFormat(date, "HH:mm:ss dd.MM.yyyy")}</span>
                </div>
            </div>
            <div className="chat-msg__content">
                <span className="chat-msg__text">{message}</span>
            </div>
        </div>
    )
}

export default ChatMessage;