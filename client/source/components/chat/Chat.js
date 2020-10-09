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
    const s = {
        messages: (fake_messages) ? fake_messages : [],
        room_name: 'Палата №1'
    };

    const [state, setState] = useState(s);
    const historyRef = React.useRef();

    const onNewMessage = data => {
        console.log("Message data = ", data);
        setState({room_name: state.room_name, messages: [...state.messages, data]});
        historyRef.current.scrollTop = historyRef.current.scrollHeight;
    }

    const initSockets = () => {
        Socket.on('chat', onNewMessage);
        Socket.on('joinroom', (data) => {
            console.log("Chat joinroom data = ", data);
            console.log("room name = ", data.data.room);
            if (data.successful) {
                setState({
                    messages: [],
                    room_name: data.data.room
                });
            }
        });
    }

    const cleanSockets = () => {
        Socket.off('chat');
        Socket.off('joinroom');
    }

    useEffect(() => {
        initSockets();
        historyRef.current.scrollTop = historyRef.current.scrollHeight;
        return cleanSockets;
    }, [onNewMessage]);

    return (
        <div className="chat">
            <RoomHeader name={state.room_name}></RoomHeader>
            <div className="chat__history-wrapper">
                <div ref={historyRef} className="chat__history">
                    { state.messages.map((msg, index) => <ChatMessage key={index} msg={msg}></ChatMessage>) }
                </div>
            </div>
            <ChatInput></ChatInput>
        </div>
    )
}

export default Chat;