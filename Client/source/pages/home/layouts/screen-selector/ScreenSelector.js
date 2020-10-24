import React from 'react';

import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import * as FontAwesomeIcons from '@fortawesome/free-solid-svg-icons';

import './ScreenSelectors.scss';

const ScreenSelector = () => {
  return (
    <div className="scrn-selector">
      <div className="scrn-selector__item"></div>
      <div className="scrn-selector__item">
        <FontAwesomeIcon icon={FontAwesomeIcons.faUsers}></FontAwesomeIcon>
      </div>
    </div>
  )
}

export default ScreenSelector;
