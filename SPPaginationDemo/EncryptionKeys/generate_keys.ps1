openssl genpkey -algorithm RSA -out private_key.pem

openssl rsa -pubout -in private_key.pem -out public_key.pem