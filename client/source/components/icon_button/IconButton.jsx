import React from 'react';
import PropTypes from 'prop-types';

import { IconButton as BaseIconButton } from '@material-ui/core';
import classNames from 'classnames';

import './IconButton.scss';

const IconButton = ({ className, disabled, ...props }) => (
  <div className="">
    <BaseIconButton
      className={classNames('icon-btn', className, {
        'disabled': disabled,
      })}
      {...props}
    />
  </div>
);

IconButton.defaultProps = {
  className: '',
  disabled: false,
};

IconButton.propTypes = {
  className: PropTypes.string,
  disabled: PropTypes.bool,
};

export default IconButton;
