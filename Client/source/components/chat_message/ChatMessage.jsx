import React from 'react';
import PropTypes from 'prop-types';

import dateFnsFormat from 'date-fns/format';

import { Message } from 'logic';

import './ChatMessage.scss';

const ChatMessage = ({ msg, ...props }) => {
  console.log("ChatMessage msg = ", msg);
  const { date, content, author } = msg;
  console.log("Date = ", date);
  console.log("Content = ", content);
  console.log("Author = ", author);
  return (
    <div className="chat-msg">
      <div className="chat-msg__info">
        <img className="chat-msg__av" alt="avatar" src={author.avatar} />
        <div className="chat-msg__name-date">
          <span className="chat-msg__name">{`${author.id}`}</span>
          <span className="chat-msg__date" title={date}>{dateFnsFormat(date, 'HH:mm:ss dd.MM.yyyy')}</span>
        </div>
      </div>
      <div className="chat-msg__content">
        <span className="chat-msg__text">{content.text}</span>
      </div>
    </div>
  );
};

ChatMessage.propTypes = {
  msg: PropTypes.instanceOf(Message).isRequired,
};

export default ChatMessage;
