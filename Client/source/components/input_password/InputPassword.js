import React, { useState } from 'react';

import classNames from 'class-names';
import { Input, InputAdornment } from '@material-ui/core';
import { Visibility, VisibilityOff } from '@material-ui/icons';

import { IconButton } from 'components';
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
            onClick={handleClickShowPassword}
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
