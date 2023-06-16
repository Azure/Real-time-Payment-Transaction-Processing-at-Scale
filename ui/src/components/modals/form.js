import { Modal } from 'flowbite-react';

const FormModal = ({ children, header, setOpenModal, openModal }) => {
  return (
    <Modal show={openModal} size="md" popup onClose={() => setOpenModal(false)}>
      <Modal.Header className="items-center">{header}</Modal.Header>
      <Modal.Body>{children}</Modal.Body>
    </Modal>
  );
};

export default FormModal;
