import React from 'react';

import classNames from 'class-names';
import { withStyles } from '@material-ui/core';

import './LabelButton.scss';

const LabelButton = ({ className, button, label }) => {
  return (
    <div className={classNames(className, "label-btn")}>
      <div className="label-btn__label">
        {label}
      </div>
      {button}
    </div>
  );
};

export default LabelButton;
