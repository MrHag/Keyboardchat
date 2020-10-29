import React from 'react';
import { useHistory } from 'react-router-dom';

import { Button } from 'components';
import { Socket, UserData } from 'logic';
import { ROUTES } from 'shared';

import './UserPanel.scss';

const UserPanel = () => {
  const routeHistory = useHistory();

  const onLogoutButton = () => {
    Socket.emit('deAuth', {});
    routeHistory.push(ROUTES.Authorization.route);
    UserData.toDefault();
  };

  return (
    <div className="user-panel">
      <h3 className="user-panel__username">Username: {UserData.user.name}</h3>
      <Button onClick={onLogoutButton}>Log Out</Button>
    </div>
  );
};

export default UserPanel;
