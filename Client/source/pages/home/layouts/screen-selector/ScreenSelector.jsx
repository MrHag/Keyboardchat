import React from 'react';
import { Route, Switch, NavLink } from 'react-router-dom';

import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import * as FontAwesomeIcons from '@fortawesome/free-solid-svg-icons';

import { ROUTES } from 'shared';

import './ScreenSelectors.scss';

const ScreenSelector = () => (
  <div className="scrn-selector">
    <NavLink className="scrn-selector__item user" activeClassName="is-active" to={ROUTES.UserPanel.route}>
      <div className="item__content" />
    </NavLink>
    <NavLink className="scrn-selector__item" activeClassName="is-active" to={ROUTES.RoomChat.route}>
      <div className="item__content">
        <FontAwesomeIcon icon={FontAwesomeIcons.faUsers} />
      </div>
    </NavLink>

  </div>
);

export default ScreenSelector;
