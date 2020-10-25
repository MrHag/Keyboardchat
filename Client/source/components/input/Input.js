import React from 'react';

import classNames from 'class-names';
import { Input as BaseInput } from '@material-ui/core';
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
    }
  },

  'input--round': {
    borderRadius: '16px',
    backgroundColor: '#222429',

    '&::before': {
      borderBottom: 'none',
      transition: 'none',
      left: '14px',
      right: '14px',
    },
  },
});

const Input = ({ className, centered, ...props }) => {
  const classes = useStyles(); 

  const class_name = classNames(classes.input, classes.root, { 
    [classes['input--round']]: props.variant === 'round',
  }, className);

  return (
    <BaseInput
      className={class_name}
      {...props}
    />
  );
};

export default Input;