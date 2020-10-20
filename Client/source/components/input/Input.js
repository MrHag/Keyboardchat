import React from 'react';

import classNames from 'class-names';
import { Input as BaseInput, InputAdornment, IconButton } from '@material-ui/core';

import './Input.scss';

const Input = ({ className, ...props }) => {
  return (
    <BaseInput
      className={classNames('input', className)}
      {...props}
    />
  )
  
};

export default Input;