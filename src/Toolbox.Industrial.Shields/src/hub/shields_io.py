class ShieldsIO:
    def __init__(self):
        pass

    def read_digital(self, request):
        return False

    def write_digital(self, request):
        return request["Value"]

    def read_analog(self, request):
        return 4095

    def write_analog(self, request):
        return request["Value"]
