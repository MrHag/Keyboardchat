import React from 'react';
import PropTypes from 'prop-types';

import classNames from 'class-names';
import { Input as BaseInput, InputAdornment } from '@material-ui/core';
import { makeStyles } from '@material-ui/core/styles';

import './Input.scss';

const HOVER_UNDERLINE_COLOR = '#FA0';
const FOCUSED_UNDERLINE_COLOR = '#0AF';

const useStyles = makeStyles({
  input: {
    backgroundColor: '#222429',
    color: '#c2c3c4',
    padding: '2px 8px',

    '&&:hover::before': {
      borderBottomColor: HOVER_UNDERLINE_COLOR,
    },

    '&&.Mui-focused::after': {
      borderBottomColor: FOCUSED_UNDERLINE_COLOR,
    },

    '&&.MuiInput-underline::after': {
      borderBottomColor: FOCUSED_UNDERLINE_COLOR,
    },

    '& .MuiInputBase-input': {
      textAlign: 'center',
    },

    '& .MuiInputAdornment-root': {
      position: 'absolute',
      right: '8px',
      width: '16px',
      height: '16px',
    },
  },

  'input--round': {
    borderRadius: '8px',
    backgroundColor: '#222429',

    '&::before': {
      borderBottom: 'none',
      transition: 'none',
      left: '6px',
      right: '6px',
    },

    '&::after': {
      left: '6px',
      right: '6px',
    },
  },
});

const Input = ({ className, variant, button, ...props }) => {
  const classes = useStyles();

  const adornment = (button) ? (
    <InputAdornment position="end">
      {button}
    </InputAdornment>
  ) : null;

  const classList = classNames('input', classes.input, {
    [classes['input--round']]: variant === 'round',
  }, className);

  return (
    <BaseInput
      className={classList}
      endAdornment={adornment}
      {...props}
    >
      {button}
    </BaseInput>
  );
};

Input.defaultProps = {
  className: '',
  variant: '',
  button: null,
};

Input.propTypes = {
  button: PropTypes.node,
  className: PropTypes.string,
  variant: PropTypes.string,
};

export default Input;
