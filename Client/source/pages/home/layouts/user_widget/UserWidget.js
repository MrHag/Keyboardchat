import React, { useState } from 'react';
import { useHistory } from 'react-router-dom';

import classNames from 'class-names';

import { Button } from 'components';
import { Socket } from 'logic';

import './UserWidget.scss';

const UserWidget = () => {
  const [toggled, setToggled] = useState(false);
  const routerHistory = useHistory();

  const onUserWidgetClick = () => {
    setToggled(!toggled);
  }

  const onLogoutButton = () => {
    Socket.emit('deauth', {});
    routerHistory.push('/');
  }

  const toggle = toggled ? (
    <div className={classNames("user-widget__toggle", { "toggled": toggled })}>
      <Button onClick={onLogoutButton}>Log out</Button>
    </div>
  ) : null;

  return (
      <div
        className="user-widget"
        onClick={onUserWidgetClick}
      >
        <div className="user-widget__info">
          <div className="user-widget__av" />
          <div className="user-widget__name">User Login</div>
        </div>
        {toggle}
      </div>
  );
};

export default UserWidget;