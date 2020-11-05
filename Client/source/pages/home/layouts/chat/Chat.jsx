import React, { useEffect, useState } from 'react';
import PropTypes from 'prop-types';

import { ChatMessage, ChatInput } from 'components';
import { UserData, Message, Socket, SocketManager } from 'logic';

import ChatHeader from './chat_header/ChatHeader';
import './Chat.scss';

const ChatHistory = ({ messages, historyRef }) => {
  const historyMessages = messages.map(
    (msg, index) => <ChatMessage key={index + msg.author.id} msg={msg} />,
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

ChatHistory.propTypes = {
  messages: PropTypes.arrayOf(PropTypes.shape).isRequired,
  historyRef: PropTypes.any.isRequired,
  // historyRef: PropTypes.oneOfType([
  //   PropTypes.func,
  //   PropTypes.shape({ current: PropTypes.shape }),
  // ]).isRequired,
};

const Chat = () => {
  const [state, setState] = useState({
    messages: [],
    room_name: 'Палата №1',
  });
  const historyRef = React.useRef();

  const socketJoinRoom = (result) => {
    if (result.error === null) {
      setState({
          messages: [],
          room_name: result.data.room,
      });
    }
  };

  const socketLeaveroom = () => {
    setState({
      messages: [],
      room_name: 'Палата №1',
    });
  };

  const onNewMessage = (result) => {
    if (result.error === null) {
      const msg = result.data;
      setState({ room_name: state.room_name, messages: [...state.messages, msg] });
      historyRef.current.scrollTop = historyRef.current.scrollHeight;
    }
  }

  const initSockets = () => {
    SocketManager.addCallback('onNewMsg', onNewMessage);
    SocketManager.addCallback('joinRoom', socketJoinRoom);
    SocketManager.addCallback('leaveRoom', socketLeaveroom);
  };

  const cleanSockets = () => {
    SocketManager.removeCallback('onNewMsg', onNewMessage);
    SocketManager.removeCallback('joinRoom', socketJoinRoom);
    SocketManager.removeCallback('leaveRoom', socketLeaveroom);
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
