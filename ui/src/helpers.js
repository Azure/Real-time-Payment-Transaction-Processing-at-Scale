export const USDollar = new Intl.NumberFormat('en-US', {
  style: 'currency',
  currency: 'USD'
});

export const Capitalize = (word) => {
  return word.charAt(0).toUpperCase() + word.slice(1);
};
