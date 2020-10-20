import React, { useState } from 'react';

import classNames from 'class-names';
import { Input, InputAdornment, IconButton } from '@material-ui/core';
import { Visibility, VisibilityOff } from '@material-ui/icons';

import './InputPassword.scss';

const InputPassword = ({ className, ...props }) => {
  const [show, setShow] = useState(false);

  const handleClickShowPassword = () => {
    setShow(!show);
  };

  return (
    <Input
      type={show ? "text" : "password"}
      className={classNames('input-password', className)}
      {...props}
      endAdornment={
        <InputAdornment position="end">
          <IconButton
            aria-label="toggle password visibility"
            onClick={handleClickShowPassword}
            //onMouseDown={handleMouseDownPassword}
            edge="end"
          >
            {show ? <Visibility /> : <VisibilityOff />}
          </IconButton>
        </InputAdornment>
      }
    />
  );
};

export default InputPassword;