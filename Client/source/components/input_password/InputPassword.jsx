import React, { useState } from 'react';

import classNames from 'class-names';
import { InputAdornment } from '@material-ui/core';

import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import * as FontAwesomeIcons from '@fortawesome/free-solid-svg-icons';

import { IconButton, Input } from 'components';
import './InputPassword.scss';

const InputPassword = ({ className, ...props }) => {
  const [show, setShow] = useState(false);

  const handleClickShowPassword = () => {
    setShow(!show);
  };

  const icon = show ? (
    <FontAwesomeIcon icon={FontAwesomeIcons.faEye} />
  ) : (
    <FontAwesomeIcon icon={FontAwesomeIcons.faEyeSlash} />
  );

  return (
    <Input
      type={show ? 'text' : 'password'}
      className={classNames('input-pass', className)}
      button={(
        <InputAdornment position="end">
          <IconButton
            onClick={handleClickShowPassword}
            edge="end"
          >
            {icon}
          </IconButton>
        </InputAdornment>
      )}
      {...props}
    />
  );
};

export default InputPassword;
