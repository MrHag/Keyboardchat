import React, { useState, useEffect } from 'react';
import { useHistory } from 'react-router-dom';

import { Button, Input } from 'components';
import { Socket, SocketManager, UserData } from 'logic';
import { ROUTES } from 'shared';

import './UserPanel.scss';

const UserPanel = () => {
  const [userName, setUserName] = useState(UserData.user.name);
  const [error, setError] = useState('');

  const routeHistory = useHistory();

  const onLogoutButton = () => {
    Socket.emit('deAuth', {});
    routeHistory.push(ROUTES.Authorization.route);
    UserData.toDefault();
  };

  const onSaveButton = () => {
    Socket.emit('changeProfile', {
      name: userName,
      avatar: null,
    });
  };

  const socketChangeProfile = (result) => {
    console.log('socketChangeProfile Result = ', result);
    if (result.error !== null) {
      setError(result.error);
    }
  };

  useEffect(() => {
    SocketManager.addCallback('changeProfile', socketChangeProfile);
    return () => SocketManager.removeCallback('changeProfile', socketChangeProfile);
  }, []);

  const onUsernameChange = (e) => {
    setUserName(e.target.value);
  };

  return (
    <div className="user-panel">
      <div className="user-panel__container">
        <div className="user-panel__field">
          <label htmlFor="username">Username</label>
          <Input id="username" value={userName} onChange={onUsernameChange} />
        </div>
        <span className="user-panel__error">
          {error}
        </span>
        <Button className="user-panel__save" onClick={onSaveButton}>Save</Button>
      </div>
      <Button className="user-panel__logout" onClick={onLogoutButton}>Log Out</Button>
    </div>
  );
};

export default UserPanel;
