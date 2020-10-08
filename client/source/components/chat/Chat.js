import React, { useEffect, useState } from 'react';

import Socket from '../../Socket';
import { ChatMessage, ChatInput } from '../index';

import './Chat.scss';

const fake_messages = undefined;//require('../../../fake_data/fake.json').chat_messages;

const Chat = () => {
    //Stores messages in JSON format
    const [messages, setMessages] = useState((fake_messages) ? fake_messages : []);
    const historyRef = React.useRef();

    const onNewMessage = data => {
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