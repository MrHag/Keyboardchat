import React from 'react';

import classNames from 'classnames';
import { Button as BaseButton, withStyles } from '@material-ui/core';

import './Button.scss';

const Button = (props) => {
    const disabled = props.disabled;
    return (
        <BaseButton 
            className={classNames("button", {"disabled": disabled})} 
            variant="contained"
            {...props}
        />
    )
}

export default Button;