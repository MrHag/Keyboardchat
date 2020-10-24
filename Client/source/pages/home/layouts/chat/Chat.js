import React, { useEffect, useReducer, useState } from 'react';
import PropTypes from 'prop-types';

import { ChatMessage, ChatInput, IconButton } from 'components';
import { Socket } from 'logic';

import ChatHeader from './chat_header/ChatHeader';

import './Chat.scss';

try {
    var fake_messages = require('fake_data/fake.json').chat_messages;
} catch (err) { }

const Chat = () => {
    if (fake_messages) {
        for (let msg of fake_messages) {
            msg.date = new Date();
            msg.date.setSeconds(Math.random() * 1000);
        }
    } 

    const [state, setState] = useState( {
        messages: (fake_messages) ? fake_messages : [],
        room_name: 'Палата №1'
    });
    const historyRef = React.useRef();

    const onNewMessage = data => {
        data.date = new Date();
        setState({room_name: state.room_name, messages: [...state.messages, data]});
        historyRef.current.scrollTop = historyRef.current.scrollHeight;
    }

    const socketJoinroom = data => {
        // console.log("Chat joinroom data = ", data);
        // console.log("room name = ", data.data.room);
        if (data.successful) {
            setState({
                messages: [],
                room_name: data.data.room
            });
        }
    };

    const socketLeaveroom = () => {
        setState({
            messages: [],
            room_name: 'Палата №1'
        });
    };

    const initSockets = () => {
        Socket.on('chat', onNewMessage);
        Socket.on('joinroom', socketJoinroom);
        Socket.on('leaveroom', socketLeaveroom);
    }

    const cleanSockets = () => {
        Socket.removeEventListener('chat', onNewMessage);
        Socket.removeEventListener('joinroom', socketJoinroom);
        Socket.removeEventListener('leaveroom', socketLeaveroom);
    }

    useEffect(() => {
        initSockets();
        historyRef.current.scrollTop = historyRef.current.scrollHeight;
        return cleanSockets;
    }, [onNewMessage]);

    return (
        <div className="chat">
            <RoomHeader name={state.room_name}></RoomHeader>``
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