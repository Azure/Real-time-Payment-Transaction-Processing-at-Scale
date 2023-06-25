import _ from 'lodash';

export const USDollar = new Intl.NumberFormat('en-US', {
  style: 'currency',
  currency: 'USD'
});

export const Capitalize = (word) => {
  return word.charAt(0).toUpperCase() + word.slice(1);
};

export const DiffObjects = (obj1, obj2) => {
  const diff = _.differenceWith(_.toPairs(obj1), _.toPairs(obj2), _.isEqual);
  return diff
    .map(([key, value]) => ({
      [key]: value
    }))
    .reduce((r, c) => Object.assign(r, c), {});
};
