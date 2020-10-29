import React from 'react';

import classNames from 'classnames';

import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import * as FontAwesomeIcons from '@fortawesome/free-solid-svg-icons';
import { IconButton } from '../index';

import './Sidebar.scss';

const Sidebar = ({ children, ...props }) => {
  const [isHidden, setHidden] = React.useState(false);

  const onArrowBtnHandler = (e) => {
    setHidden(!isHidden);
  };

  const icon = (isHidden) ? FontAwesomeIcons.faAngleRight : FontAwesomeIcons.faAngleLeft;

  return (
    <div className={classNames('sidebar')} {...props}>
      <IconButton
        className={classNames('sidebar__arrow-btn', { 'hidden': isHidden })}
        onClick={onArrowBtnHandler}
      >
        <FontAwesomeIcon icon={icon} />
      </IconButton>
      <div
        className={classNames('sidebar__content', { 'hidden': isHidden })}
      >
        {children}
      </div>
    </div>
  );
};

export default Sidebar;
