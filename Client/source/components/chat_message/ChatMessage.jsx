import React from 'react';
import PropTypes from 'prop-types';

import dateFnsFormat from 'date-fns/format';

import './ChatMessage.scss';

const ChatMessage = ({ 
  avatarUrl,
  authorName,
  text,
  date,
 }) => {
  return (
    <div className="chat-msg">
      <div className="chat-msg__info">
        <img className="chat-msg__av" alt="avatar" src={avatarUrl} />
        <div className="chat-msg__name-date">
          <span className="chat-msg__name">{`${authorName}`}</span>
          <span className="chat-msg__date" title={date}>{dateFnsFormat(date, 'HH:mm:ss dd.MM.yyyy')}</span>
        </div>
      </div>
      <div className="chat-msg__content">
        <span className="chat-msg__text">{text}</span>
      </div>
    </div>
  );
};

ChatMessage.propTypes = {
  avatarUrl: PropTypes.string.isRequired,
  authorName: PropTypes.string.isRequired,
  text: PropTypes.string.isRequired,
  date: PropTypes.instanceOf(Date).isRequired,
};

export default ChatMessage;
