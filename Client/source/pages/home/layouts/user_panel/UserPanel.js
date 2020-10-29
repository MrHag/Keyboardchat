import React from 'react';
import { useHistory } from 'react-router-dom';

import { Button } from 'components';
import { Socket } from 'logic';
import { ROUTES } from 'shared';

import './UserPanel.scss';

const UserPanel = () => {
  const routeHistory = useHistory();

  const onLogoutButton = () => {
    Socket.emit('deAuth', {});
    routeHistory.push(ROUTES.Authorization.route);
  }

  return (
    <div className="user-panel">
      <Button onClick={onLogoutButton}>Log Out</Button>
    </div>
  )
};

export default UserPanel;
