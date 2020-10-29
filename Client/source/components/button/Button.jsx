import React from 'react';
import PropTypes from 'prop-types';

import classNames from 'classnames';
import { Button as BaseButton, withStyles } from '@material-ui/core';

import './Button.scss';

const styles = {
  root: {
    '&:hover': {
      backgroundColor: '#50b1ff',
    },
    '&:disabled': {
      backgroundColor: '#758897',
    },
    backgroundColor: '#6fafe3',

    '&&.disabled': {
      color: 'red',
    },
  },
};

const Button = (props) => {
  const { disabled } = props;
  return (
    <BaseButton
      {...props}
      variant="contained"
      className={classNames('button', { 'disabled': disabled })}
    />
  );
};

Button.defaultProps = {
  disabled: false,
};

Button.propTypes = {
  disabled: PropTypes.bool,
};

export default withStyles(styles)(Button);
