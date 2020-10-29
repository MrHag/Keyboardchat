import React from 'react';
import PropTypes from 'prop-types';

import classNames from 'class-names';
import { withStyles } from '@material-ui/core';

import './LabelButton.scss';

const LabelButton = ({ className, button, label }) => (
  <div className={classNames(className, 'label-btn')}>
    <div className="label-btn__label">
      {label}
    </div>
    {button}
  </div>
);

LabelButton.defaultProps = {
  className: '',
};

LabelButton.propTypes = {
  className: PropTypes.string,
  button: PropTypes.node,
  label: PropTypes.string.isRequired,
};

export default LabelButton;
