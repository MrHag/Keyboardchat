import React from 'react';
import PropTypes from 'prop-types';

import classNames from 'classnames';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import * as FontAwesomeIcons from '@fortawesome/free-solid-svg-icons';

import { IconButton } from 'components';
import './Sidebar.scss';

const Sidebar = ({ children }) => {
  const [isHidden, setHidden] = React.useState(false);

  const onArrowBtnHandler = () => {
    setHidden(!isHidden);
  };

  const icon = (isHidden) ? FontAwesomeIcons.faAngleRight : FontAwesomeIcons.faAngleLeft;

  return (
    <div className={classNames('sidebar')}>
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

Sidebar.propTypes = {
  children: PropTypes.oneOfType([
    PropTypes.arrayOf(PropTypes.node),
    PropTypes.node,
  ]).isRequired,
};

export default Sidebar;
