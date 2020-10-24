import React from 'react';

import { IconButton as BaseIconButton } from '@material-ui/core';

import classNames from 'classnames';

import './IconButton.scss';

const IconButton = ({ className, ...props }) => {
    return (
        <BaseIconButton
            className={classNames('icon-btn', className, {'disabled': props.disabled})}
            {...props}
        />
    );
}

export default IconButton;