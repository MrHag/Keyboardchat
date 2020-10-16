import React from 'react';

import './Sidebar.scss';

import { IconButton } from '../index';
import classNames from 'classnames';

import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import * as FontAwesomeIcons from '@fortawesome/free-solid-svg-icons';

const Sidebar = ({children, ...props}) => {
    const [isHidden, setHidden] = React.useState(false);

    const onArrowBtnHandler = (e) => {
        setHidden(!isHidden);
    }

    const icon = (isHidden) ? FontAwesomeIcons.faArrowRight : FontAwesomeIcons.faArrowLeft;

    return (
        <div className={classNames("sidebar", {"hidden": isHidden})} {...props}>
            <IconButton onClick={onArrowBtnHandler} className="sidebar__arrow-btn">
                <FontAwesomeIcon icon={icon}></FontAwesomeIcon>
            </IconButton>
            {(isHidden) ? null : children}
        </div>
    )
}

export default Sidebar;