import React, { useEffect, useState } from 'react';
import PropTypes from 'prop-types';

import { ChatMessage, ChatInput } from 'components';
import { Socket } from 'logic';

import ChatHeader from './chat_header/ChatHeader';

import './Chat.scss';
import UserData from '../../../../logic/UserData';

try {
  var fake_messages = null; // require('fake_data/fake.json').chat_messages;
} catch (err) { }

const ChatHistory = ({ messages, historyRef }) => {
  const historyMessages = messages.map(
    (msg, index) => <ChatMessage key={index} msg={msg} />,
  );

  return (
    <>
      <div className="chat__history-wrapper">
        <div ref={historyRef} className="chat__history">
          {historyMessages}
        </div>
      </div>
    </>
  );
};

const Chat = () => {
  if (fake_messages) {
    for (const msg of fake_messages) {
      msg.date = new Date();
      msg.date.setSeconds(Math.random() * 1000);
    }
  }

  const [state, setState] = useState({
    messages: (fake_messages) || [],
    room_name: 'Палата №1',
  });
  const historyRef = React.useRef();

  const onNewMessage = (data) => {
    data.date = new Date();
    console.log('OnNewMessage data = ', data);
    setState({ room_name: state.room_name, messages: [...state.messages, data] });
    historyRef.current.scrollTop = historyRef.current.scrollHeight;
  };

  const socketJoinroom = (data) => {
    if (data.successful) {
      setState({
        messages: [],
        room_name: data.data.room,
      });
    }
  };

  const socketLeaveroom = () => {
    setState({
      messages: [],
      room_name: 'Палата №1',
    });
  };

  const initSockets = () => {
    Socket.on('chat', onNewMessage);
    Socket.on('joinRoom', socketJoinroom);
    Socket.on('leaveRoom', socketLeaveroom);
  };

  const cleanSockets = () => {
    Socket.removeEventListener('chat', onNewMessage);
    Socket.removeEventListener('joinRoom', socketJoinroom);
    Socket.removeEventListener('leaveRoom', socketLeaveroom);
  };

  useEffect(() => {
    initSockets();
    historyRef.current.scrollTop = historyRef.current.scrollHeight;
    return cleanSockets;
  }, [onNewMessage]);

  return (
    <div className="chat">
      <ChatHeader name={UserData.inRoom.name} />
      <ChatHistory
        historyRef={historyRef}
        messages={state.messages}
      />
      <ChatInput />
    </div>
  );
};

export default Chat;
