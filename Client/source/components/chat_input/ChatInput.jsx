import React, { useState } from 'react';

import { TextareaAutosize } from '@material-ui/core/';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import * as FontAwesomeIcons from '@fortawesome/free-solid-svg-icons';

import { IconButton } from 'components';
import { Socket } from 'logic';

import './ChatInput.scss';

const ChatInput = () => {
  const [text, setText] = useState('');

  const sendMessage = () => {
    setText('');
    Socket.emit('chat', {
      message: text,
    });
  };

  const onChatKeydown = (e) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      if (text.length) {
        sendMessage();
      }
    }
  };

  return (
    <div className="chat-input">
      <TextareaAutosize
        className="chat-input__area"
        variant="outlined"
        rowsMax={3}
        rowsMin={3}
        value={text}
        autoFocus
        placeholder="Your message"
        onChange={(e) => setText(e.target.value)}
        onKeyDown={(e) => onChatKeydown(e)}
      />
      <div className="chat-input__controls">
        <IconButton
          className="chat-input__send"
          disabled={text === ''}
          onClick={sendMessage}
        >
          <FontAwesomeIcon icon={FontAwesomeIcons.faPaperPlane} />
        </IconButton>
      </div>
    </div>
  );
};

export default ChatInput;
