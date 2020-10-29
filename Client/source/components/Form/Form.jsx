import React from 'react';
import PropTypes from 'prop-types';

import './Form.scss';

const Form = ({ name, children, ...props }) => (
  <div className="form">
    <h2 className="form__name">{name}</h2>
    {children}
  </div>
);

Form.propTypes = {
  name: PropTypes.string.isRequired,
};

export default Form;
