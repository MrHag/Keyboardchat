import React, { useEffect, useState } from 'react';
import { useHistory } from 'react-router-dom';
import PropTypes from 'prop-types';

import { Input, InputPassword, Button, Form } from 'components';
import { Socket, SocketManager, UserData } from 'logic';
import { ROUTES } from 'shared';

import './CreationRoom.scss';

const CreationRoom = ({ onRoomCreate }) => {
  const [name, setName] = useState('');
  const [password, setPassword] = useState('');
  const [err, setErr] = useState('');
  const routeHistory = useHistory();
  /*
    Пока joinroom всегда возвращает success. Если это изменится,
    то нужно будет хендлить ошибки здесь
  */
  const onCreateBtn = () => {
    Socket.emit('createRoom', {
      name,
      password: (password.length === 0) ? null : password,
    });
  };

  const onCancelBtn = () => {
    routeHistory.goBack();
  };

  const socketCreateRoom = (result) => {
    if (result.error === null) {
      console.log('Result.data = ', result.data);
      UserData.setInRoomJSON(result.data);
      routeHistory.push(ROUTES.RoomChat.route);
      onRoomCreate();
    }
  };

  useEffect(() => {
    SocketManager.addCallback('createRoom', socketCreateRoom);
    return () => SocketManager.removeCallback('createRoom', socketCreateRoom);
  }, []);

  const onInputKeydown = (e) => {
    if (e.key === 'Enter') {
      onCreateBtn();
    }
  };

  return (
    <div className="creation-room">
      <div className="creation-room__bg" />
      <div className="creation-room__content">
        <Form
          name="Create room"
        >
          <Input
            onChange={(e) => setName(e.target.value)}
            className="creation-room__input"
            placeholder="Room name"
            onKeyDown={onInputKeydown}
            autoFocus
          />
          <InputPassword
            onChange={(e) => setPassword(e.target.value)}
            className="creation-room__input"
            placeholder="Room password"
            onKeyDown={onInputKeydown}
          />
          <p className="creation-room__error form__error">{err}</p>
          <div className="creation-room__buttons form__controls">
            <Button onClick={onCancelBtn}>Cancel</Button>
            <Button disabled={name === ''} onClick={onCreateBtn}>Create</Button>
          </div>
        </Form>
      </div>
    </div>
  );
};

CreationRoom.propTypes = {
  onRoomCreate: PropTypes.func.isRequired,
};

export default CreationRoom;
