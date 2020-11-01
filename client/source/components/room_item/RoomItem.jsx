import React, { useEffect, useReducer, useState } from 'react';
import PropTypes from 'prop-types';
import classNames from 'classnames';

import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import * as FontAwesomeIcons from '@fortawesome/free-solid-svg-icons';

import { Button, IconButton, InputPassword } from 'components';
import { Socket, Room } from 'logic';

import './RoomItem.scss';

const RoomItemForm = ({ roomId, onCancel, onJoin }) => {
  const [password, setPassword] = useState('');
  const [err, setErr] = useState('');

  const socketJoinroom = (data) => {
    if (data.successful) {
      onJoin();
    } else {
      // Here might be other types of errors
      setErr('Invalid password!');
    }
  };

  const onJoinBtn = () => {
    const request = {
      id: roomId,
      password,
    };
    console.log('request = ', request);
    Socket.emit('joinRoom', request);
  };

  useEffect(() => {
    Socket.on('joinRoom', socketJoinroom);
    return () => Socket.removeEventListener('joinRoom', socketJoinroom);
  }, []);

  return (
    <div className="ri-form">
      <InputPassword
        className="ri-form__password"
        placeholder="Password"
        onChange={(e) => setPassword(e.target.value)}
        onKeyDown={(e) => {
          if (e.key === 'Enter') onJoinBtn();
        }}
        autoFocus
      />
      <p className="ri-form__error">{err}</p>
      <div
        className="ri-form__buttons"
      >
        <Button onClick={onCancel}>Cancel</Button>
        <Button disabled={password === ''} onClick={onJoinBtn}>Join</Button>
      </div>
    </div>
  );
};

RoomItemForm.propTypes = {
  roomId: PropTypes.number.isRequired,
  onCancel: PropTypes.func.isRequired,
  onJoin: PropTypes.func.isRequired,
};

const RoomItem = ({ active, roomData, onRoomJoin, onRoomLeave }) => {
  const [stage, setStage] = useState('');

  const onClickHandler = (e) => {
    if (!active) {
      if (roomData.haspass) {
        if (stage !== 'joining') {
          setStage('joining');
        }
      } else {
        onRoomJoin(roomData, null);
      }
    }
  };

  const onJoin = () => {
    setStage('');
  };

  let form = null;
  if (stage === 'joining') {
    form = (
      <RoomItemForm
        roomId={roomData.id}
        onCancel={() => { setStage(''); }}
        onJoin={onJoin}
      />
    );
  }

  const roomLeaveButton = active && (
    <IconButton
      className="room-item__leave-btn"
      onClick={onRoomLeave}
      title="Leave room"
    >
      <FontAwesomeIcon
        className="room-item__lock-icon"
        icon={FontAwesomeIcons.faTimesCircle}
      />
    </IconButton>
  );
  const { haspass, name } = { ...roomData };
  return (
    <div onClick={onClickHandler} className={classNames('room-item', { 'active': active })}>
      <div className="room-item__content">
        {haspass && <FontAwesomeIcon className="room-item__lock-icon" icon={FontAwesomeIcons.faLock} />}
        <div className="room-item__name" title={name}>{name}</div>
        {roomLeaveButton}
      </div>
      {form}
    </div>
  );
};

RoomItem.propTypes = {
  roomData: PropTypes.instanceOf(Room).isRequired,
  active: PropTypes.bool.isRequired,
  onRoomJoin: PropTypes.func.isRequired,
  onRoomLeave: PropTypes.func.isRequired,
};

export default RoomItem;
