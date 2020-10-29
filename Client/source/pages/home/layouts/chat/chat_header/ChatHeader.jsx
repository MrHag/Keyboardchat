import React from 'react';
import PropTypes from 'prop-types';

import './ChatHeader.scss';

const ChatHeader = ({ name }) => {
  const roomName = (name === 'global') ? 'Палата №1' : name;
  return (
    <div className="chat-header">
      <div className="chat-header__wrapper">
        <h3 className="chat-header__name">{roomName}</h3>
      </div>
    </div>
  );
};

ChatHeader.propTypes = {
  name: PropTypes.string.isRequired,
};

export default ChatHeader;
