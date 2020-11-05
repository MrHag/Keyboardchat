import React from 'react';
import { NavLink } from 'react-router-dom';

import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import * as FontAwesomeIcons from '@fortawesome/free-solid-svg-icons';

import { ROUTES } from 'shared';
import { UserData } from 'logic';

import './ScreenSelectors.scss';

const UserSelector = () => {
  const Domen = 'http://localhost:3000/'
  
  const userAvatar = (UserData.user !== null && UserData.user.avatar) ? (
    <p>Av</p>
  ) : (
    <img className="s-sel__item__img" src={`${Domen}avatar/1`} />
  );

  return (
    <NavLink className="s-selector user" activeClassName="is-active" to={ROUTES.UserPanel.route}>
        <div className="s-selector__content">
          {userAvatar}
        </div>
    </NavLink>
  );
}

const ScreenSelector = () => {
  return (
    <div className="s-selectors">
      <UserSelector></UserSelector>
      <NavLink className="s-selector" activeClassName="is-active" to={ROUTES.RoomChat.route}>
        <div className="s-selector__content">
          <FontAwesomeIcon icon={FontAwesomeIcons.faUsers} />
        </div>
      </NavLink>
    </div>
  );
};

export default ScreenSelector;
