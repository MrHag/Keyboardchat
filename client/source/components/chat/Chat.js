import React, { useEffect, useReducer, useState } from 'react';
import PropTypes from 'prop-types';

import Socket from '../../Socket';
import { ChatMessage, ChatInput } from '../index';

import './Chat.scss';

try {
    var fake_messages = require('../../../fake_data/fake.json').chat_messages;
} catch (err) { }

const RoomHeader = ({name}) => {
    const room = (name === 'global') ? 'Палата №1' : name;
    return (
        <div className="room-header">
            <h3 className="room-header__name">{room}</h3>
        </div>
    )
};

RoomHeader.propTypes = {
    name: PropTypes.string.isRequired
};

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
    } );
    const historyRef = React.useRef();

    const onNewMessage = data => {
        data.date = new Date();
        setState({room_name: state.room_name, messages: [...state.messages, data]});
        historyRef.current.scrollTop = historyRef.current.scrollHeight;
    }

    const socketJoinroom = data => {
        console.log("Chat joinroom data = ", data);
            console.log("room name = ", data.data.room);
            if (data.successful) {
                setState({
                    messages: [],
                    room_name: data.data.room
                });
            }
    } 

    const initSockets = () => {
        Socket.on('chat', onNewMessage);
        Socket.on('joinroom', socketJoinroom);
    }

    const cleanSockets = () => {
        Socket.removeEventListener('chat', onNewMessage);
        Socket.removeEventListener('joinroom', socketJoinroom);
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