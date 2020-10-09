import React, { useEffect, useState } from 'react';
import PropTypes from 'prop-types';

import Socket from '../../Socket';
import { ChatMessage, ChatInput } from '../index';

import './Chat.scss';

try {
    var fake_messages = require('../../../fake_data/fake.json').chat_messages;
} catch (err) { }

const RoomHeader = ({name}) => {
    return (
        <div className="room-header">
            <h3 className="room-header__name">{name}</h3>
        </div>
    )
};

RoomHeader.propTypes = {
    name: PropTypes.string.isRequired
};

const Chat = () => {
    //Stores messages in JSON format
    const [messages, setMessages] = useState((fake_messages) ? fake_messages : []);
    const historyRef = React.useRef();

    const onNewMessage = data => {
        //console.log("Message data = ", data);
        setMessages([...messages, data]);
        historyRef.current.scrollTop = historyRef.current.scrollHeight;
    }

    useEffect(() => {
        Socket.on('chat', onNewMessage);
        historyRef.current.scrollTop = historyRef.current.scrollHeight;
        return () => Socket.off('chat', onNewMessage);
    });

    return (
        <div className="chat">
            <RoomHeader name="Палата №1"></RoomHeader>
            <div className="chat__history-wrapper">
                <div ref={historyRef} className="chat__history">
                    { messages.map((msg, index) => <ChatMessage key={index} msg={msg}></ChatMessage>) }
                </div>
            </div>
            <ChatInput></ChatInput>
        </div>
    )
}

export default Chat;